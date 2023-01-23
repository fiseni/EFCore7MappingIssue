using System.Runtime.CompilerServices;
using EFCore7MappingIssue.Infrastructure;

namespace EFCore7MappingIssue.Domain;

public class TriggerType : BaseEnum<TriggerType>
{
    public static readonly TriggerType UpdateEmail = new(1, "Update of email", 0, false, "Update of email");
    public static readonly TriggerType UpdateMobileNumber = new(2, "Update of mobile number", 1, false, "Update of mobile number");
    public static readonly TriggerType AccountSignedUp = new(3, "Account signed up", 2, false, "Account signed up");
    public static readonly TriggerType ServiceHistoryLinkedToVehicle = new(4, "Service history added to a linked vehicle", 3, false, "Service history added to a linked vehicle");
    public static readonly TriggerType VehicleRelationshipEnded = new(5, "Vehicle relationship ended", 4, false, "Vehicle relationship ended");
    public static readonly TriggerType CustomerInvite = new(6, "Customer invited by retailer staff member", 4, false, "Customer invite");

    private TriggerType(int value, string description, int position, bool hasRunTimeDefined, [CallerMemberName] string name = "")
        : base(value, name)
    {
        Description = description;
        Position = position;
        HasRunTimeDefined = hasRunTimeDefined;
    }

    public string Description { get; private set; }
    public int Position { get; private set; }
    public bool HasRunTimeDefined { get; private set; }
}
