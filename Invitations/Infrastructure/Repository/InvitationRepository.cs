using Core.Interfaces;
using Core.Models;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public class InvitationRepository(ApplicationDbContext context) : IInvitationRepository
{
    private readonly DbSet<Invitation> _invitations = context.Invitations;
    // Get all invitations
    // Get invitation by id
    // Get all based on predicate
    // Get one invitation based on predicate
    // Create invitation
    // Update invitation
    public async Task<IEnumerable<Invitation>> GetAllAsync()
    {
        return await _invitations.AsNoTracking().ToListAsync();
    }

    public async Task<Invitation?> GetByIdAsync(int id)
    {
        return await _invitations.FindAsync(id);
    }
    public async Task CreateAsync(Invitation invitation)
    {
        await _invitations.AddAsync(invitation);
    }

    public void Delete(Invitation invitation)
    {
        _invitations.Remove(invitation);
    }
    public void Update(Invitation invitation)
    {
        _invitations.Update(invitation);
    }

    public async Task<Invitation?> Find(Expression<Func<Invitation, bool>> predicate)
    {
        return await _invitations.FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<Invitation?>> FindAll(Expression<Func<Invitation, bool>> predicate)
    {
        return await _invitations.Where(predicate).ToListAsync();
    }
}
