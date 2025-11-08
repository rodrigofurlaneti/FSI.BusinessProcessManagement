using FSI.BusinessProcessManagement.Application.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Fakes
{
    public sealed class InMemoryGenericAppService<TDto> : IGenericAppService<TDto>
        where TDto : class
    {
        private readonly System.Func<TDto, long> _getId;
        private readonly System.Action<TDto, long> _setId;
        private readonly System.Func<TDto, TDto, TDto> _applyUpdate;
        private long _nextId = 1;
        private readonly ConcurrentDictionary<long, TDto> _store = new();

        public InMemoryGenericAppService(
            System.Func<TDto, long> getId,
            System.Action<TDto, long> setId,
            System.Func<TDto, TDto, TDto> applyUpdate)
        {
            _getId = getId;
            _setId = setId;
            _applyUpdate = applyUpdate;
        }

        public Task<IEnumerable<TDto>> GetAllAsync()
            => Task.FromResult<IEnumerable<TDto>>(_store.Values.ToList());

        public Task<TDto?> GetByIdAsync(long id)
        {
            _store.TryGetValue(id, out var dto);
            return Task.FromResult(dto);
        }

        public Task<long> InsertAsync(TDto dto)
        {
            var id = _nextId++;
            _setId(dto, id);
            _store[id] = dto;
            return Task.FromResult(id);
        }

        public Task UpdateAsync(TDto dto)
        {
            var id = _getId(dto);
            if (_store.TryGetValue(id, out var current))
            {
                var merged = _applyUpdate(current, dto);
                _store[id] = merged;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(long id)
        {
            _store.TryRemove(id, out _);
            return Task.CompletedTask;
        }
    }
}