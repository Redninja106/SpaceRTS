namespace SpaceGame.Economy;

class ResourceValues
{
    public int Consumption;
    public int Capacity;

    public int Remaining => Capacity - Consumption;
}