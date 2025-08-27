namespace OdixPay.Notifications.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool IsActive { get; protected set; } = true;
    public bool IsDeleted { get; protected set; } = false;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    public void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted(bool? @boolean = true)
    {
        IsDeleted = @boolean ?? true;
    }

    public void MarkAsActive(bool? @boolean = true)
    {
        IsActive = @boolean ?? true;
    }
}
