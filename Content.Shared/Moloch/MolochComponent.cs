using Robust.Shared.GameStates;

namespace Content.Shared.Moloch;

/// <summary>
/// Компонент для данных Молоха.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MolochComponent : Component
{
    /// <summary>
    /// Время в секундах до следующего рывка.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChargeCooldown = 5f;

    /// <summary>
    /// Текущий таймер до следующего рывка.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChargeTimer = 0f;

    /// <summary>
    /// Скорость во время рывка.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChargeSpeed = 8f;

    /// <summary>
    /// Длительность рывка в секундах.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ChargeDuration = 1.5f;

    /// <summary>
    /// Флаг, находится ли Молох в состоянии рывка.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsCharging = false;
}