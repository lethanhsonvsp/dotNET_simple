using RosbridgeNet.RosbridgeClient.Common.Attributes;
using Newtonsoft.Json;


[RosMessageType("geometry_msgs/msg/PoseWithCovarianceStamped")]
public class PoseWithCovarianceStamped
{
    public Header? header { get; set; }
    public PoseWithCovariance? pose { get; set; }

    public override string ToString()
    {
        return $"header: {header}, pose: {pose}";
    }
}

public class Header
{
    public Stamp? stamp { get; set; }
    public string? frame_id { get; set; }

    public override string ToString()
    {
        return $"stamp: {stamp}, frame_id: {frame_id}";
    }
}

public class Stamp
{
    public int sec { get; set; }
    public int nanosec { get; set; }

    public override string ToString()
    {
        return $"sec: {sec}, nanosec: {nanosec}";
    }
}

public class PoseWithCovariance
{
    public Pose? pose { get; set; }
    public double[] covariance { get; set; }

    public override string ToString()
    {
        var cov = string.Join(", ", covariance);
        return $"pose: {pose}, covariance: [{cov}]";
    }
}

public class Pose
{
    public Position? position { get; set; }
    public Orientation? orientation { get; set; }

    public override string ToString()
    {
        return $"position: {position}, orientation: {orientation}";
    }
}

public class Position
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }

    public override string ToString()
    {
        return $"x: {x}, y: {y}, z: {z}";
    }
}

public class Orientation
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }
    public double w { get; set; }

    public override string ToString()
    {
        return $"x: {x}, y: {y}, z: {z}, w: {w}";
    }
}
