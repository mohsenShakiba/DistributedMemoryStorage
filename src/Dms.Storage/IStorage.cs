using System;
using System.Threading.Tasks;

namespace Dms.Storage
{
    public interface IStorage
    {
        ValueTask InitializeAsync();
        ValueTask<Memory<byte>> ReadAsync(string key);
        ValueTask WriteAsync(string key, Memory<byte> value);
        ValueTask DeleteAsync(string key);
        ValueTask VacuumAsync();
        ValueTask<VacuumStat> VacuumStatAsync();
    }
}