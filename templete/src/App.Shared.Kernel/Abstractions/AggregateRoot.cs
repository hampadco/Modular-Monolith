using System;

namespace App.Shared.Kernel.Abstractions;

/// <summary>
/// پایه برای تمام موجودیت‌های دامنه.
/// </summary>
public abstract class AggregateRoot<TId>
{
    public TId Id { get; protected set; }
    
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

