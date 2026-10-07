using System;

namespace Core.Domain;

public sealed class Order
{
    public string Id { get; }
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }

    private Order(string id, string customerId, OrderStatus status)
    {
        Id = id;
        CustomerId = customerId;
        Status = status;
    }

    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id замовлення обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Id клієнта обов'язковий", nameof(customerId));

        // Нове замовлення завжди стартує у стані Draft
        return new Order(id.Trim(), customerId.Trim(), OrderStatus.Draft);
    }

    // Підтвердження замовлення
    public void Confirm() => ChangeStatus(OrderStatus.Confirmed);

    // Скасування замовлення
    public void Cancel() => ChangeStatus(OrderStatus.Cancelled);

    // Єдиний метод переходу, який перевіряє матрицю станів через switch expression
    private void ChangeStatus(OrderStatus targetStatus)
    {
        // Перевірка допустимих переходів між станами
        bool isTransitionAllowed = (Status, targetStatus) switch
        {
            // З Draft можна перейти в Confirmed або Cancelled
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,

            // З Confirmed можна скасувати
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,

            // Повторний перехід у той самий стан заборонено
            var (current, next) when current == next => false,

            // Будь-які інші переходи (наприклад, з Cancelled назад у Confirmed/Draft) заборонені
            _ => false
        };

        if (!isTransitionAllowed)
        {
            throw new InvalidOperationException(
                $"Неможливо перевести замовлення {Id} зі стану '{Status}' у стан '{targetStatus}'.");
        }

        Status = targetStatus;
    }

    public override string ToString() => $"Замовлення {Id} (Клієнт: {CustomerId}) -> Стан: {Status}";
}