using Opc.Ua;
using Opc.Ua.Server;

public class MyNodeManagerRos2 : CustomNodeManager2
{
    private BaseDataVariableState? cmdVelVariable;
    private BaseDataVariableState? odomVariable;
    private BaseDataVariableState? scanVariable;
    private BaseDataVariableState? amclPoseVariable;
    private BaseDataVariableState? mapVariable;
    private BaseDataVariableState? tfVariable;
    private BaseDataVariableState? tfStaticVariable;
    private BaseDataVariableState? imuVariable;
    private BaseDataVariableState? jointStatesVariable;

    public MyNodeManagerRos2(IServerInternal server, ApplicationConfiguration configuration)
        : base(server, configuration, "http://yourorganization.org/SimpleOpcUaServer/")
    {
    }

    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        base.CreateAddressSpace(externalReferences);

        // Tạo một folder cho robot
        var robotFolder = new FolderState(null)
        {
            SymbolicName = "Robot",
            ReferenceTypeId = ReferenceTypeIds.Organizes,
            TypeDefinitionId = ObjectTypeIds.FolderType,
            NodeId = new NodeId("Robot", NamespaceIndex),
            BrowseName = new QualifiedName("Robot", NamespaceIndex),
            DisplayName = new LocalizedText("Robot"),
            Description = new LocalizedText("Robot"),
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None
        };

        AddPredefinedNode(SystemContext, robotFolder);

        // Tạo các biến cho các topic ROS2
        cmdVelVariable = CreateVariable(robotFolder, "cmd_vel", "Command Velocity", BuiltInType.String, ValueRanks.Scalar);
        // Tạo biến    =                Thư mục gốc + Định danh + Tên hiển thị + Kiểu dữ liệu + Cấp giá trị (scalar)
        odomVariable = CreateVariable(robotFolder, "odom", "Odometry", BuiltInType.String, ValueRanks.Scalar);
        scanVariable = CreateVariable(robotFolder, "scan", "Laser Scan", BuiltInType.String, ValueRanks.Scalar);
        amclPoseVariable = CreateVariable(robotFolder, "amcl_pose", "AMCL Pose", BuiltInType.String, ValueRanks.Scalar);
        mapVariable = CreateVariable(robotFolder, "map", "Map", BuiltInType.String, ValueRanks.Scalar);
        tfVariable = CreateVariable(robotFolder, "tf", "Transformations", BuiltInType.String, ValueRanks.Scalar);
        tfStaticVariable = CreateVariable(robotFolder, "tf_static", "Static Transformations", BuiltInType.String, ValueRanks.Scalar);
        imuVariable = CreateVariable(robotFolder, "imu", "IMU Data", BuiltInType.String, ValueRanks.Scalar);
        jointStatesVariable = CreateVariable(robotFolder, "joint_states", "Joint States", BuiltInType.String, ValueRanks.Scalar);

        // Thêm các biến vào không gian địa chỉ
        AddPredefinedNode(SystemContext, cmdVelVariable);
        AddPredefinedNode(SystemContext, odomVariable);
        AddPredefinedNode(SystemContext, scanVariable);
        AddPredefinedNode(SystemContext, amclPoseVariable);
        AddPredefinedNode(SystemContext, mapVariable);
        AddPredefinedNode(SystemContext, tfVariable);
        AddPredefinedNode(SystemContext, tfStaticVariable);
        AddPredefinedNode(SystemContext, imuVariable);
        AddPredefinedNode(SystemContext, jointStatesVariable);
    }

    private BaseDataVariableState CreateVariable(NodeState parent, string name, string description, BuiltInType dataType, int valueRank)
    {
        var variable = new BaseDataVariableState(parent)
        {
            SymbolicName = name,
            ReferenceTypeId = ReferenceTypeIds.HasComponent,
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            NodeId = new NodeId(name, NamespaceIndex),
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText(name),
            Description = new LocalizedText(description),
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None,
            DataType = (uint)dataType,
            ValueRank = valueRank,
            AccessLevel = AccessLevels.CurrentReadOrWrite,
            UserAccessLevel = AccessLevels.CurrentReadOrWrite,
            Historizing = false
        };

        return variable;
    }
}
