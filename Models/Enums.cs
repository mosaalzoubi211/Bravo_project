namespace Bravo.Models
{
    public enum TaskStatus
    {
        Pending = 1,
        Accepted = 2,
        Completed = 3,
        Cancelled = 4,
        ReceivingOffers = 5,
    }

    public enum UserRole
    {
        Client = 1,
        Worker = 2,
        Admin = 3
    }
    public enum PaymentMethod
    {
        Cash = 1,
        Electronic = 2
    }

}