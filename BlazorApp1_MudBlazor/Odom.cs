using RosbridgeNet.RosbridgeClient.Common.Attributes;

[RosMessageType("nav_msgs/msgs/Odometry")]
public class Pose
{
    public VectorO? position { get; set; }
    public VectorO? orientation { get; set; }

    public override string ToString()
    {
        return $"linear: {position}, angular: {orientation}";
    }
}

public class VectorO
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public float w { get; set; }

    public override string ToString()
    {
        return $"x: {x}, y: {y}, z: {z}, w: {w}" ;
    }
}
