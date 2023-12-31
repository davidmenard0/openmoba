public class PlayerInfo
{
    public string Name;
    public int PeerID;
    public int Team;

    public PlayerInfo Copy()
    {
        return new PlayerInfo{
            Name = this.Name,
            PeerID = this.PeerID,
            Team = this.Team
        };
    }
}