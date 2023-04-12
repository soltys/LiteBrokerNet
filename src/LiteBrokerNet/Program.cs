using LiteBrokerNet;
using Task = LiteBrokerNet.Task;
using TaskStatus = LiteBrokerNet.TaskStatus;

Console.WriteLine("Version from library is: " + LiteBroker.GetVersion());

using var broker = new LiteBroker();

broker.Send("MyQueueFromCSharp", "{}");
using var collection = broker.Receive();

Task lastTask = null;
foreach (var task in collection.Tasks)
{
    Console.WriteLine("Found new task!");
    Console.WriteLine($"Id: {task.Id}");
    Console.WriteLine($"Queue: {task.Queue}");
    Console.WriteLine($"Payload: {task.Payload}");
    Console.WriteLine($"Created: {task.Created}");
    Console.WriteLine($"Status: {task.Status}");

    lastTask = task;
}

if (lastTask != null)
{
    broker.SetStatus(lastTask.Id, TaskStatus.Acknowledged);
}