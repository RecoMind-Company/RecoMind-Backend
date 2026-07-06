using Core.DTOs.AI;
using Core.DTOs.AI.ValidationReport.AIResult;

namespace Core.DTOs;

public static class StaticAiResponse
{
    public static AIPlanDto StaticResponse = new AIPlanDto
    {
        plan_id = "plan_202606270312_aaca77",
        plan_title = "Quarterly Sales Growth Initiative (20% Increase)",
        status = "draft",
        start_date = new DateTime(2026, 6, 27),
        deadline_date = new DateTime(2026, 11, 4),
        total_estimated_days = 131,
        modules = new List<AIModuleDto>
    {
        new AIModuleDto
        {
            module_id = "mod_1",
            module_name = "Market Research and Lead Generation",
            tasks = new List<AITasksDto>
            {
                new AITasksDto
                {
                    task_id = "task_101",
                    title = "Identify Target Customer Segments",
                    description = "Conduct market research to identify high-potential customer segments for new business acquisition. Use CRM data, industry reports, and competitor analysis to refine targeting.",
                    duration_days = 7,
                    start_date = "2026-06-27",
                    deadline_date = "2026-07-03",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-tarek.nabil-employee",
                        job_title = "Sales Operations Specialist"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_102",
                    title = "Develop Lead Generation Strategy",
                    description = "Create a tailored lead generation strategy for each identified customer segment, including sourcing channels (cold outreach, digital ads, referrals, etc.).",
                    duration_days = 5,
                    start_date = "2026-07-04",
                    deadline_date = "2026-07-08",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-ahmed.sales-teamleader",
                        job_title = "Sales Team Leader"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_103",
                    title = "Compile Lead Lists",
                    description = "Compile and organize lead lists based on the strategy, ensuring accuracy and segmentation for targeted outreach. Use CRM tools to manage leads.",
                    duration_days = 3,
                    start_date = "2026-07-09",
                    deadline_date = "2026-07-11",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-tarek.nabil-employee",
                        job_title = "Sales Operations Specialist"
                    }
                }
            }
        },
        new AIModuleDto
        {
            module_id = "mod_2",
            module_name = "Outreach and Initial Engagement",
            tasks = new List<AITasksDto>
            {
                new AITasksDto
                {
                    task_id = "task_104",
                    title = "Initial Cold Outreach to Leads",
                    description = "Perform cold calls/emails to leads using personalized scripts. Schedule initial meetings or presentations with potential customers.",
                    duration_days = 10,
                    start_date = "2026-07-12",
                    deadline_date = "2026-07-21",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-aya.mansour-employee",
                        job_title = "Telemarketing Agent"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_105",
                    title = "Follow-Up with Potential Customers",
                    description = "Follow up with leads who showed initial interest, addressing objections and nurturing relationships to move them closer to a sale.",
                    duration_days = 12,
                    start_date = "2026-07-22",
                    deadline_date = "2026-08-02",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-mahmoud.ali-employee",
                        job_title = "B2B Sales Representative"
                    }
                }
            }
        },
        new AIModuleDto
        {
            module_id = "mod_3",
            module_name = "Product Presentations and Proposals",
            tasks = new List<AITasksDto>
            {
                new AITasksDto
                {
                    task_id = "task_106",
                    title = "Prepare Customized Product Presentations",
                    description = "Develop tailored product presentations for different customer segments, highlighting key benefits and addressing pain points.",
                    duration_days = 8,
                    start_date = "2026-08-03",
                    deadline_date = "2026-08-10",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-yasmine.ali-employee",
                        job_title = "Sales Account Manager"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_107",
                    title = "Conduct Product Demonstrations",
                    description = "Deliver live product demonstrations to potential customers, either in-person or virtually, and gather feedback.",
                    duration_days = 10,
                    start_date = "2026-08-11",
                    deadline_date = "2026-08-20",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-mahmoud.ali-employee",
                        job_title = "B2B Sales Representative"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_108",
                    title = "Generate and Send Proposals",
                    description = "Create and send customized proposals to qualified leads, including pricing, terms, and next steps for negotiation.",
                    duration_days = 7,
                    start_date = "2026-08-21",
                    deadline_date = "2026-08-27",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-kareem.salah-employee",
                        job_title = "Sales Executive"
                    }
                }
            }
        },
        new AIModuleDto
        {
            module_id = "mod_4",
            module_name = "Sales Negotiation and Closing",
            tasks = new List<AITasksDto>
            {
                new AITasksDto
                {
                    task_id = "task_109",
                    title = "Negotiate Contract Terms",
                    description = "Engage in contract negotiations with potential customers, addressing concerns and finalizing terms that are mutually beneficial.",
                    duration_days = 15,
                    start_date = "2026-08-28",
                    deadline_date = "2026-09-11",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-yasmine.ali-employee",
                        job_title = "Sales Account Manager"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_110",
                    title = "Close Sales Deals",
                    description = "Finalize sales deals by securing signatures, processing orders, and ensuring smooth onboarding for new customers.",
                    duration_days = 12,
                    start_date = "2026-09-12",
                    deadline_date = "2026-09-23",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-maha.ali-employee",
                        job_title = "Inside Sales Representative"
                    }
                }
            }
        },
        new AIModuleDto
        {
            module_id = "mod_5",
            module_name = "Customer Onboarding and Support",
            tasks = new List<AITasksDto>
            {
                new AITasksDto
                {
                    task_id = "task_111",
                    title = "Onboard New Customers",
                    description = "Provide new customers with onboarding support, including training, setup, and ensuring they are satisfied with the product/service.",
                    duration_days = 10,
                    start_date = "2026-09-24",
                    deadline_date = "2026-10-03",
                    status = "to_do",
                    priority = "low",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-reem.hassan-employee",
                        job_title = "Customer Success Specialist"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_112",
                    title = "Monitor Customer Satisfaction",
                    description = "Track customer satisfaction through surveys, check-ins, and feedback loops to ensure long-term success and retention.",
                    duration_days = 5,
                    start_date = "2026-10-04",
                    deadline_date = "2026-10-08",
                    status = "to_do",
                    priority = "low",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-reem.hassan-employee",
                        job_title = "Customer Success Specialist"
                    }
                }
            }
        },
        new AIModuleDto
        {
            module_id = "mod_6",
            module_name = "Performance Reporting and Analysis",
            tasks = new List<AITasksDto>
            {
                new AITasksDto
                {
                    task_id = "task_113",
                    title = "Track Sales Metrics in Real-Time",
                    description = "Monitor daily/weekly sales metrics (e.g., conversion rates, deal sizes, lead response times) using CRM and analytics tools.",
                    duration_days = 15,
                    start_date = "2026-10-09",
                    deadline_date = "2026-10-23",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-hany.adel-employee",
                        job_title = "Sales Support Assistant"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_114",
                    title = "Prepare Quarterly Sales Report",
                    description = "Compile a detailed report on sales performance, including key indicators (e.g., revenue growth, customer acquisition cost, conversion rates).",
                    duration_days = 7,
                    start_date = "2026-10-24",
                    deadline_date = "2026-10-30",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-tarek.nabil-employee",
                        job_title = "Sales Operations Specialist"
                    }
                },
                new AITasksDto
                {
                    task_id = "task_115",
                    title = "Analyze Performance and Identify Trends",
                    description = "Analyze sales data to identify trends, strengths, and areas for improvement. Provide actionable insights for future strategies.",
                    duration_days = 5,
                    start_date = "2026-10-31",
                    deadline_date = "2026-11-04",
                    status = "to_do",
                    priority = "high",
                    suggested_owner = new AISuggestedOwnersDto
                    {
                        user_id = "Sales-omar.kamal-employee",
                        job_title = "Senior Sales Specialist"
                    }
                }
            }
        }
    }
    };

    public static TaskResponseDto StaticValidationReport = new TaskResponseDto
    {
        TaskId = "559f605a-c7dc-4816-9243-4ac299535955",
        Status = "SUCCESS",
        Error = null,
        Result = new TaskResultDto
        {
            ValidationReport = new ValidationReportDto
            {
                ExecutiveSummary = "The validation report assesses the feasibility of increasing company sales by 20% in the next quarter through targeting new customers, following up with potential customers, conducting product presentations, negotiating contracts, completing sales, and preparing a final report. The precedent analysis shows a high context match level score and a confidence score of 93.88, indicating that similar strategies have been successful in the past. However, the resource simulation results indicate that the company's human resources are insufficient, and financial data is lacking, which may hinder the plan's execution.",
                ValidationDecision = "Conditional",
                ConfidenceScore = 80,
                KeyFindings = new KeyFindingsDto
                {
                    PrecedentAnalysis = "The precedent analysis reveals a high context match level score of 'High' and a confidence score of 93.88, based on the analysis of 4 cases. The outcomes of these cases show 3 successes, 0 partial successes, and 1 failure. The successful cases include Beardbrand, which turned a growing community into an eCommerce store, generating $20,000 in sales every day; Ancient + Brave, which achieved £20m in sales in three years; and Beer Cartel, which used content marketing to increase sales. The failed case is Allbirds, which closed all its U.S. stores due to a failed retail strategy. The what_worked patterns include content marketing, growing community, and eCommerce store, while the what_failed patterns include closed decision, decision close, and full price. The key insights highlight the importance of leveraging community, creating a successful eCommerce platform, and using content marketing as a growth engine. The context match level score and confidence score suggest that the planned strategy has a high likelihood of success, given the similarities between the precedent cases and the current company context.",

                    // Mapped smoothly to string as per your class configuration
                    ResourceAssessment = "The resource assessment indicates that the company's financial resources are unknown due to a lack of financial data, with reasons including the absence of specific financial data in the provided reports and the inability to determine financial capacity without concrete data. The human resources are insufficient, with a status of 'Insufficient' due to reasons such as a demotivated sales team with a significant performance gap, an average quota achievement rate of 9.88%, and a high turnover risk. The key metrics for human resources include a sales team size of 17 and an average quota achievement rate of 9.88%. In contrast, the operational resources are ready, with a status of 'Ready' due to the company's existing operational setup as an e-commerce company. The overall execution verdict is that the company cannot execute the plan due to blocking factors such as insufficient human resources and a lack of financial data.",

                    MarketTrends = "The market trend analysis shows a growing market direction with a growth rate of 18.7%-24.54% and a trend confidence score of 90. The timing assessment is favorable due to high growth rates and increasing online sales. The key trends include generative AI and zero-click search, media networks driving incremental growth, and secondhand and sustainable e-commerce. The opportunities include expansion into new markets such as India and Vietnam, investment in AI-powered e-commerce platforms, and development of sustainable and secondhand e-commerce offerings. The risks include intense competition in mature markets, regulatory challenges and data privacy concerns, and dependence on technology and infrastructure. The location insights indicate a global market with emerging markets in India and Vietnam, a growing market maturity, and a medium competition level. The recommendation is to invest in e-commerce platforms and AI-powered technologies to capitalize on growing demand and increasing online sales."
                },
                Recommendations = new List<string>
                {
                "Invest in e-commerce platforms and AI-powered technologies to capitalize on growing demand and increasing online sales",
                "Develop a comprehensive financial plan to address the lack of financial data and ensure sufficient financial resources",
                "Implement strategies to motivate and retain the sales team, such as performance-based incentives and training programs",
                "Expand into new markets such as India and Vietnam to diversify revenue streams and reduce dependence on mature markets"
                },
                RiskFactors = new List<string>
                {
                "Insufficient human resources due to a demotivated and potentially high-turnover sales team",
                "Lack of financial data to confirm the company's ability to support the plan's financial requirements",
                "Intense competition in mature markets",
                "Regulatory challenges and data privacy concerns"
                },
                NextSteps = new List<string>
                {
                "Conduct a thorough financial analysis to determine the company's financial capacity and identify potential funding sources",
                "Develop a detailed plan to address the sales team's performance gap and retention risks",
                "Research and evaluate potential e-commerce platforms and AI-powered technologies for investment",
                "Establish a task force to monitor and address regulatory challenges and data privacy concerns"
                }
            }
        }
    };
}
