using System.Drawing;
using System.Runtime.InteropServices;

namespace LiteBrokerNet;

public class LiteBrokerNative
{
    private const string DllName = "liteBroker";

    [DllImport(LiteBrokerNative.DllName)]
    public static extern IntPtr broker_version();

    [DllImport(LiteBrokerNative.DllName)]
    public static extern int broker_initialize(out IntPtr broker);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern int broker_destroy(IntPtr broker);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern int broker_send(IntPtr broker, string queue, string payload);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern int broker_receive(IntPtr broker, out IntPtr collection);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern int broker_finalize(IntPtr collection);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern int broker_task_count(IntPtr collection);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern IntPtr broker_task_at(IntPtr collection, int index);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern IntPtr broker_task_get_id(IntPtr collection);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern IntPtr broker_task_get_payload(IntPtr collection);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern IntPtr broker_task_get_queue(IntPtr collection);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern IntPtr broker_task_get_created(IntPtr collection);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern int broker_task_get_status(IntPtr collection);

    [DllImport(LiteBrokerNative.DllName)]
    public static extern int broker_set_status(IntPtr broker, IntPtr idStr, int status);
}

public enum BrokerResult : int
{
    Ok = 0,
    Failed = 1,
}

public class LiteBroker : IDisposable
{
    private readonly IntPtr brokerPtr;

    public LiteBroker()
    {
        this.brokerPtr = IntPtr.Zero;
        var result = (BrokerResult)LiteBrokerNative.broker_initialize(out this.brokerPtr);
        if (result != BrokerResult.Ok)
        {
            throw new InvalidOperationException(
                $"{nameof(LiteBrokerNative.broker_initialize)} returned not Ok result");
        }
    }

    private void ReleaseUnmanagedResources()
    {
        LiteBrokerNative.broker_destroy(this.brokerPtr);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~LiteBroker()
    {
        ReleaseUnmanagedResources();
    }


    public static string GetVersion()
    {
        var ptr = LiteBrokerNative.broker_version();
        var versionString = Marshal.PtrToStringAnsi(ptr);

        if (versionString == null)
        {
            throw new Exception("Cannot get version string from LiteBroker Native");
        }

        return versionString;
    }

    public void Send(string queue, string payload)
    {
        LiteBrokerNative.broker_send(this.brokerPtr, queue, payload);
    }

    public TaskCollection Receive()
    {
        return TaskCollection.GetFromBroker(this.brokerPtr);
    }

    public void SetStatus(string id, TaskStatus status)
    {
        var idPtr = Marshal.StringToHGlobalAnsi(id);

        LiteBrokerNative.broker_set_status(this.brokerPtr, idPtr, (int)status);

        Marshal.FreeHGlobal(idPtr);
    }
}

public class TaskCollection : IDisposable
{
    private IntPtr collectionPtr;
    private readonly int taskSize;
    public List<Task> Tasks { get; }

    private TaskCollection(IntPtr collectionPtr, int taskSize)
    {
        this.taskSize = taskSize;
        Tasks = new List<Task>(taskSize);

        this.collectionPtr = collectionPtr;
    }

    private void GetAllTasks()
    {
        for (int i = 0; i < this.taskSize; i++)
        {
            var taskPtr = LiteBrokerNative.broker_task_at(this.collectionPtr, i);

            var task = new Task
            {
                Id = Marshal.PtrToStringAnsi(LiteBrokerNative.broker_task_get_id(taskPtr)),
                Queue = Marshal.PtrToStringAnsi(LiteBrokerNative.broker_task_get_queue(taskPtr)),
                Payload = Marshal.PtrToStringAnsi(LiteBrokerNative.broker_task_get_payload(taskPtr)),
                Created = Marshal.PtrToStringAnsi(LiteBrokerNative.broker_task_get_created(taskPtr)),
                Status = LiteBrokerNative.broker_task_get_status(taskPtr)
            };

            Tasks.Add(task);
        }
    }

    public static TaskCollection GetFromBroker(IntPtr broker)
    {
        LiteBrokerNative.broker_receive(broker, out var collectionPtr);

        int size = LiteBrokerNative.broker_task_count(collectionPtr);

        var taskCollection = new TaskCollection(collectionPtr, size);
        taskCollection.GetAllTasks();
        return taskCollection;
    }

    private void ReleaseUnmanagedResources()
    {
        LiteBrokerNative.broker_finalize(this.collectionPtr);
        this.collectionPtr = IntPtr.Zero;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~TaskCollection()
    {
        ReleaseUnmanagedResources();
    }
}

public enum TaskStatus : int
{
    New = 0,
    Acknowledged = 1
}

public class Task
{
    public string Id { get; set; }
    public string Payload { get; set; }
    public int Status { get; set; }
    public string Created { get; set; }
    public string Queue { get; set; }
}