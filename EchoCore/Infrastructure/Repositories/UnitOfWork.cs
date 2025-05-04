using EchoCore.Domain.Repositories;
using EchoCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EchoCore.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MemoryDbContext _dbContext;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(MemoryDbContext dbContext, ILogger<UnitOfWork> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                _logger.LogWarning("Transaction already started; skipping BeginTransactionAsync.");
                return;
            }

            try
            {
                _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogInformation("Database transaction started.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to begin transaction.");
                throw;
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync(cancellationToken);
                    _logger.LogInformation("Database transaction committed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to commit transaction. Rolling back...");
                await RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                    _logger.LogInformation("Database transaction rolled back.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback transaction.");
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var changes = await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Database changes saved ({Count} changes).", changes);
                return changes;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save database changes.");
                throw;
            }
        }
    }
}
