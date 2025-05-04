using EchoCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoCore.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MemoryDbContext>();

            if (context.Projects.Any() || context.PromptProfiles.Any())
                return;

            var project = new Project
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Helion Main Project",
                Threads = new List<MemoryThread>
                {
                    new MemoryThread
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        ThreadName = "Initial Memory Thread",
                        SemanticEntries = new List<SemanticMemoryEntry>
                        {
                            new SemanticMemoryEntry
                            {
                                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                                Content = "First semantic memory entry",
                                Embedding = GenerateMockEmbedding()
                            },
                            new SemanticMemoryEntry
                            {
                                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                                Content = "Second semantic memory entry",
                                Embedding = GenerateMockEmbedding()
                            }
                        }
                    }
                }
            };

            await context.Projects.AddAsync(project);

            var promptProfiles = new List<PromptProfile>
            {
                new PromptProfile
                {
                    AgentType = "DataGatherer",
                    Template = "If memory is available ({memory}), collect and summarize recent project updates in Markdown format. If not, proceed based solely on the prompt: {prompt}. Please reply in the same language as the prompt."
                },
                new PromptProfile
                {
                    AgentType = "Analyzer",
                    Template = "Analyze the following context ({memory}) if available, and identify any risks, inconsistencies, or optimization points based on the prompt: {prompt}. Return your analysis in Markdown format and in the same language as the prompt."
                },
                new PromptProfile
                {
                    AgentType = "Synthesizer",
                    Template = "Using the context provided ({memory}) if available, or relying solely on the prompt ({prompt}), synthesize actionable insights and next steps. Please provide your response in Markdown format and match the language of the prompt."
                },
                new PromptProfile
                {
                    AgentType = "Communicator",
                    Template = "Draft a clear stakeholder report using available memory ({memory}) if present, or focus solely on addressing the prompt: {prompt}. Return the report in Markdown format and in the same language as the prompt."
                }
            };

            await context.PromptProfiles.AddRangeAsync(promptProfiles);

            await context.SaveChangesAsync();
        }

        private static float[] GenerateMockEmbedding()
        {
            var random = new Random();
            var embedding = new float[768];
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)(random.NextDouble() * 2 - 1);
            }
            return embedding;
        }
    }
}
