using Core.DTOs.AI;

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
}
