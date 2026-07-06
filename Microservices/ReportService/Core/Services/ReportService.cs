using Core.DTOs;
using Core.DTOs.AI;
using Core.Interfaces;
using Core.Models;
using RecoMind.Contracts.Events;
using System.Text.RegularExpressions;

namespace Core.Services;

public class ReportService(IGenerateReportService generateReportService,
                           IUnitOfWork unitOfWork,
                           IReportRepository reportRepository,
                           IFileStorageService fileStorageService,
                           IGrpcTeamService grpcTeamService,
                           IDataAssignService dataAssignService,
                           INotificationService notificationService) : IReportService
{
    public async Task<TeamToReturnDto> GetUserData(string userId)
    {
        var teamDetails = await grpcTeamService.GetTeamByUserId(userId);
        if (teamDetails is null)
            return null;
        var teamToReturnDto = new TeamToReturnDto
        {
            TeamName = teamDetails.TeamName,
            TeamId = teamDetails.TeamId,
            CompanyId = teamDetails.CompanyId
        };
        return teamToReturnDto;
    }
    public async Task<AnalysisResponseDto> CreateReportByAi(AnalysisRequestDto analysisRequest)
    {
        var generateReportInitialResponse = await generateReportService.GenerateReport(analysisRequest);
        return generateReportInitialResponse;
    }
    public async Task<AiReportResponseDto> GetReportFromAi(GetReportFromAiDto reportFromAiDto)
    {
        var generatedReportStatus = await generateReportService.GetStatus(reportFromAiDto.TaskId);
        if (generatedReportStatus.Status == Status.PENDING || generatedReportStatus.Status == Status.PROGRESS)
        {
            return new AiReportResponseDto { message = "your report is being generated." };
        }
        if (generatedReportStatus.Status == Status.STATIC)
        {
            var staticReport = await reportRepository.Find(r => r.Id == "f7518b01-fc02-4451-ae8a-145961127090");
            var staticReportContent = await fileStorageService.ReadFileAsync(staticReport.FilePath);
            return new AiReportResponseDto
            {
                Id = staticReport.Id,
                IsSuccess = true,
                AiResponse = staticReportContent,
                GeneratedDate = DateTime.UtcNow
            };
        }
        //The Status are have only 3 values PENDING, FAILURE, SUCCESS
        // FAILURE CASE HANDELD IN GENERATE REPORT SERVICE
        // PENDING CASE HANDELD ABOVE
        // SUCCESS
        var fileName = await fileStorageService.SaveFileAsync(generatedReportStatus.Result!);
        // create report model 
        // add to database
        // save changes
        // return report model
        var report = new Report
        {
            Id = Guid.NewGuid().ToString(),
            TeamId = reportFromAiDto.TeamId,
            UserId = reportFromAiDto.UserId,
            FilePath = fileName,
            FileType = ".txt",
            GeneratedDate = DateTime.Now,
            Periodic = Enum.Parse<Periodic>(reportFromAiDto.Periodic)
        };
        await reportRepository.AddAsync(report);
        await unitOfWork.Save();
        var aiReportResponse = new AiReportResponseDto
        {
            Id = report.Id,
            IsSuccess = true,
            AiResponse = generatedReportStatus.Result!,
            GeneratedDate = DateTime.UtcNow
        };

        var notification = new NotificationEventDto
        {
            ReceiverId = reportFromAiDto.UserId!,
            Title = "Your Report Is Ready Now!!",
            Message = "Your report has been generated successfully.",
        };
        await notificationService.SendNotificationAsync(notification);

        return aiReportResponse;
    }

    public async Task<DividedReportDto> GetReportById(string reportId)
    {
        var report = await reportRepository.GetByIdAsync(reportId);
        if (report is null)
            return new DividedReportDto { ErrorMessage = "there is no report with this id" };

        var reportContent = await fileStorageService.ReadFileAsync(report.FilePath);
        if (string.IsNullOrEmpty(reportContent))
            return new DividedReportDto { ErrorMessage = "there is no content for this report" };

        string shortTermPattern = @"(Short-Term Plan.*?)(?=Mid-Term Plan|$)";
        string midTermPattern = @"(Mid-Term Plan.*?)(?=Long-Term Plan|$)";
        string longTermPattern = @"(Long-Term Plan.*)";

        string shortTermText = Regex.Match(reportContent, shortTermPattern, RegexOptions.Singleline).Value;
        string midTermText = Regex.Match(reportContent, midTermPattern, RegexOptions.Singleline).Value;
        string longTermText = Regex.Match(reportContent, longTermPattern, RegexOptions.Singleline).Value;

        // 2. بناء الـ DTO النهائي واستدعاء الـ Parser الداخلي
        var dividedReportDto = new DividedReportDto
        {
            Id = report.Id,
            GeneratedDate = report.GeneratedDate,
            TeamId = report.TeamId,
            ShortTerm = ParsePlanDetails(shortTermText),
            MidTerm = ParsePlanDetails(midTermText),
            LongTerm = ParsePlanDetails(longTermText)
        };

        return dividedReportDto;
    }


    public async Task<string> DeleteReport(string reportId)
    {
        var report = await reportRepository.GetByIdAsync(reportId);
        if (report is null)
            return "";
        reportRepository.Delete(report);
        await unitOfWork.Save();
        return "Report deleted successfully";
    }
    public async Task<IEnumerable<ReportDto>> GetAllReportsByTeamId(string TeamId, int limit = 2)
    {
        //Fetch all reports for the given TeamId from the repository WITH IT'S CONTENT

        var date = DateTime.UtcNow.AddDays(-7); // Example: Fetch reports generated in the last 7 days
        var reports = await reportRepository.FindAllWithLimit(r => r.TeamId == TeamId && r.GeneratedDate >= date, limit);
        if (!reports.Any())
        {
            return Enumerable.Empty<ReportDto>();
        }

        const int maxParallelTasks = 5;

        var semaphore = new SemaphoreSlim(maxParallelTasks);

        var tasks = reports.Select(async r =>
        {
            // wait for a slot from the semaphore
            await semaphore.WaitAsync();
            try
            {
                // Read file execution
                var content = await fileStorageService.ReadFileAsync(r.FilePath);

                // Create the DTO
                return new ReportDto
                {
                    Id = r.Id,
                    TeamId = r.TeamId,
                    UserId = r.UserId,
                    Periodic = r.Periodic.ToString(),
                    GeneratedDate = r.GeneratedDate,
                    Content = content
                };
            }
            finally
            {
                // Release the slot after completion (in both cases: success or error)
                semaphore.Release();
            }
        }).ToList(); // Convert to list to ensure all Select executions begin

        // Wait for all tasks to complete
        var reportsDto = await Task.WhenAll(tasks);

        return reportsDto;

        //var reports = await reportRepository.FindAll(r => r.TeamId == TeamId);

        //if (!reports.Any())
        //{
        //    return Enumerable.Empty<ReportDto>();
        //}
        //var reportsDto = reports.Select(r => new ReportDto
        //{
        //    Id = r.Id,
        //    TeamId = r.TeamId,
        //    UserId = r.UserId,
        //    Periodic = r.Periodic.ToString(),
        //    GeneratedDate = r.GeneratedDate
        //}).ToList();
        //return reportsDto;
    }

    public async Task<string> AssignCompanyData(string companyId)
    {
        var taskId = await dataAssignService.DataAssign(companyId);
        return taskId;
    }
    public async Task<string> GetAssignCompanyDataStatus(string taskId)
    {
        var status = await dataAssignService.DataAssignResult(taskId);
        return status;
    }

    public async Task<ReportDto> CreateTestReport(TestDto testDto)
    {
        var fileName = await fileStorageService.SaveFileAsync(testDto.content!);
        var report = new Report
        {
            Id = Guid.NewGuid().ToString(),
            TeamId = testDto.TeamId,
            UserId = testDto.UserId,
            FilePath = fileName,
            FileType = ".txt",
            GeneratedDate = DateTime.Now,
            Periodic = Periodic.Weekly
        };

        await reportRepository.AddAsync(report);
        await unitOfWork.Save();

        var reportContent = await fileStorageService.ReadFileAsync(report.FilePath);
        return new ReportDto
        {
            Content = reportContent,
            GeneratedDate = report.GeneratedDate,
            Id = report.Id,
            Periodic = report.Periodic.ToString(),
            TeamId = report.TeamId,
            UserId = report.UserId
        };
    }
    private PlanDetailsDto ParsePlanDetails(string planText)
    {
        var details = new PlanDetailsDto();
        if (string.IsNullOrEmpty(planText)) return details;

        string goalPattern = @"\*\*Goal:\*\*(.*?)(?=\*\*Analysis:\*\*|-\s*\*\*Recommendations:?|\*\*Recommendations:?|$)";
        string analysisPattern = @"\*\*Analysis:\*\*(.*?)(?=-\s*\*\*Recommendations:?|\*\*Recommendations:?|$)";
        string recsPattern = @"\*\*Recommendations / Actions:\*\*(.*?)(?=-\s*\*\*Scenarios:?|\*\*Scenarios:?|-\s*\*\*Reasoning:?|\*\*Reasoning:?|$)";

        // تظبيط الـ Lookahead هنا عشان يقف لو لقى سطر جديد بيبدأ بـ رقم بين نجمتين زي **2. أو #### 2.
        string scenariosPattern = @"\*\*Scenarios:\*\*(.*?)(?=-\s*\*\*Risk Management:?|\*\*Risk Management:?|-\s*\*\*Reasoning:?|\*\*Reasoning:?|\n+(?:#### |\*\*)\d+|$|$)";
        string riskManagementPattern = @"\*\*Risk Management:\*\*(.*?)(?=-\s*\*\*Reasoning:?|\*\*Reasoning:?|\n+(?:#### |\*\*)\d+|$|$)";
        string reasoningPattern = @"\*\*Reasoning:\*\*(.*?)(?=\n+(?:#### |\*\*)\d+|$|$)";

        // ميثود التنظيف الأساسية
        string CleanText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string trimmed = input.Trim();
            if (trimmed.EndsWith("-"))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 1).Trim();
            }
            return trimmed;
        }

        // ميثود جديدة مخصوص لتنظيف الـ Markdown (النجوم والـ Dashes الزيادة في أول السطور)
        string CleanMarkdown(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            // 1. تنظيف النص الأساسي
            string cleaned = CleanText(input);

            // 2. نشيل النجوم تماماً من النص
            cleaned = cleaned.Replace("**", "");

            // 3. لو السطور بتبدأ بـ مسافات وشرطة (like: "   -  ") بنخليها شكلها منظم ونشيل الفراغات
            cleaned = Regex.Replace(cleaned, @"(?<=\n|^)\s*-\s*", "- ");

            return cleaned.Trim();
        }

        List<string> SplitRecommendations(string recsText)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(recsText)) return list;

            string titlePattern = @"(?:\d+\.\s+|-)\s*\*\*(.*?)\*\*";
            var matches = Regex.Matches(recsText, titlePattern);

            int index = 1;
            foreach (Match match in matches)
            {
                string title = match.Groups[1].Value.Trim();
                if (title.EndsWith(":"))
                {
                    title = title.Substring(0, title.Length - 1).Trim();
                }

                if (!string.IsNullOrEmpty(title))
                {
                    list.Add($"{index}. {title}");
                    index++;
                }
            }
            return list;
        }

        details.Goal = CleanText(Regex.Match(planText, goalPattern, RegexOptions.Singleline).Groups[1].Value);
        details.Analysis = CleanText(Regex.Match(planText, analysisPattern, RegexOptions.Singleline).Groups[1].Value);

        string rawRecs = Regex.Match(planText, recsPattern, RegexOptions.Singleline).Groups[1].Value;
        details.Recommendations = SplitRecommendations(rawRecs);

        // بنستخدم CleanMarkdown هنا للأقسام دي عشان تطلع صافية
        var scenariosMatch = Regex.Match(planText, scenariosPattern, RegexOptions.Singleline);
        details.Scenarios = scenariosMatch.Success ? CleanMarkdown(scenariosMatch.Groups[1].Value) : null;

        var riskMatch = Regex.Match(planText, riskManagementPattern, RegexOptions.Singleline);
        details.RiskManagement = riskMatch.Success ? CleanMarkdown(riskMatch.Groups[1].Value) : null;

        var reasoningMatch = Regex.Match(planText, reasoningPattern, RegexOptions.Singleline);
        details.Reasoning = reasoningMatch.Success ? CleanMarkdown(reasoningMatch.Groups[1].Value) : null;

        return details;
    }

}
