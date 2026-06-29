using System.Linq.Expressions;

namespace Core.ServicesAbstractions;

public interface IBackgroundService
{
    void ExecuteInBackground(Expression<Action> job);
}
