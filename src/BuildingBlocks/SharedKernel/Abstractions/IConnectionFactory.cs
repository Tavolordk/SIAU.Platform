using System.Data;
namespace SharedKernel.Abstractions;
public interface IConnectionFactory { IDbConnection Create(); }
