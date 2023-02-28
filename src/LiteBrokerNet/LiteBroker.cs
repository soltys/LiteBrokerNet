using System.Drawing;
using System.Runtime.InteropServices;

namespace LiteBrokerNet;
public class LiteBrokerNative
{
    private const string DllName = "liteBroker";
    [DllImport(DllName)]
    public static extern IntPtr broker_version();

    [DllImport(DllName)]
    public static extern int broker_initialize(out IntPtr broker);

    [DllImport(DllName)]
    public static extern int broker_destroy(IntPtr broker);
    [DllImport(DllName)]
    public static extern int broker_send(IntPtr broker, string queue, string payload);

    [DllImport(DllName)]
    public static extern int broker_receive(IntPtr broker, out IntPtr collection);

    [DllImport(DllName)]
    public static extern int broker_finalize(IntPtr collection);

    [DllImport(DllName)]
    public static extern int broker_task_count(IntPtr collection);

    [DllImport(DllName)]
    public static extern IntPtr broker_task_at(IntPtr collection, int index);
    [DllImport(DllName)]
    public static extern IntPtr broker_task_get_id(IntPtr collection);
    [DllImport(DllName)]
    public static extern IntPtr broker_task_get_payload(IntPtr collection);
    [DllImport(DllName)]
    public static extern IntPtr broker_task_get_queue(IntPtr collection);
    [DllImport(DllName)]
    public static extern IntPtr broker_task_get_created(IntPtr collection);
    [DllImport(DllName)]
    public static extern int broker_task_get_status(IntPtr collection);
    [DllImport(DllName)]
    public static extern int broker_set_status(IntPtr broker, IntPtr idStr, int status);
}

public class LiteBroker : IDisposable
{
    private readonly nint brokerPtr;

    public static string GetVersion()
    {
        var ptr = LiteBrokerNative.broker_version();
        var versionString =  Marshal.PtrToStringAnsi(ptr);

        if (versionString == null)
        {
            throw new Exception("Cannot get version string from LiteBroker Native");
        }

        return versionString;
    }

    public LiteBroker()
    {
        this.brokerPtr = IntPtr.Zero;
        var result = LiteBrokerNative.broker_initialize(out brokerPtr);
    }

    public void Send(string queue, string payload)
    {
        LiteBrokerNative.broker_send(this.brokerPtr, queue, payload);
    }

    public TaskCollection Receive()
    {
       return TaskCollection.GetFromBroker(this.brokerPtr);
    }

    public void SetStatus(string id, int status)
    {
        var idPtr = Marshal.StringToHGlobalAnsi(id);

        LiteBrokerNative.broker_set_status(this.brokerPtr, idPtr, status);

        Marshal.FreeHGlobal(idPtr);
    }

    private void ReleaseUnmanagedResources()
    {
        LiteBrokerNative.broker_destroy(brokerPtr);
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
}

public class TaskCollection: IDisposable
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
        for (int i = 0; i < taskSize; i++)
        {
            var taskPtr = LiteBrokerNative.broker_task_at(collectionPtr, i);

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

public class Task
{
    public string Id { get; set; }
    public string Payload { get; set; }
    public int Status { get; set; }
    public string Created { get; set; }
    public string Queue { get; set; }
}