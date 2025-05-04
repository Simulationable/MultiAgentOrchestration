using System;
using System.Threading.Tasks;
using EchoCore.Domain;
using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Domain.Services;
using Microsoft.Extensions.Logging;

namespace EchoCore.Application.Services
{
    public class PromptProfileService : IPromptProfileService
    {
        private readonly IPromptProfileRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PromptProfileService> _logger;

        public PromptProfileService(
            IPromptProfileRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<PromptProfileService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<string> GetPromptTemplateAsync(string agentType, CancellationToken cancellationToken = default)
        {
            var profile = await _repository.GetByAgentTypeAsync(agentType.ToLower());

            if (profile == null)
                throw new KeyNotFoundException($"Prompt profile '{agentType}' not found.");

            if (profile.Template == null)
                throw new InvalidOperationException($"Prompt profile '{agentType}' has no template.");

            return profile.Template;
        }

        public async Task<bool> CreatePromptProfileAsync(string agentType, string template, CancellationToken cancellationToken = default)
        {
            var profile = new PromptProfile
            {
                AgentType = agentType.ToLower(),
                Template = template
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                await _repository.AddAsync(profile);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Created PromptProfile for agentType: {AgentType}", agentType);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create PromptProfile for agentType: {AgentType}", agentType);
                throw;
            }
        }

        public async Task<bool> UpdatePromptProfileAsync(string agentType, string updatedTemplate, CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var profile = await _repository.GetByAgentTypeAsync(agentType.ToLower());
                if (profile == null)
                    throw new KeyNotFoundException($"Prompt profile '{agentType}' not found.");

                profile.Template = updatedTemplate;
                await _repository.UpdateAsync(profile);

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Updated PromptProfile for agentType: {AgentType}", agentType);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to update PromptProfile for agentType: {AgentType}", agentType);
                throw;
            }
        }

        public async Task<bool> DeletePromptProfileAsync(string agentType, CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var profile = await _repository.GetByAgentTypeAsync(agentType.ToLower());
                if (profile == null)
                    throw new KeyNotFoundException($"Prompt profile '{agentType}' not found.");

                await _repository.DeleteAsync(profile);

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Deleted PromptProfile for agentType: {AgentType}", agentType);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to delete PromptProfile for agentType: {AgentType}", agentType);
                throw;
            }
        }
    }
}
