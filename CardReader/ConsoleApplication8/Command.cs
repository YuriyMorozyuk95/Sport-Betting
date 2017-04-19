namespace IDCardReader
{

    public enum Operation : int
    {
        ActivateChip = 1,
        TransferData = 2,
        CheckPosition = 3,
        SaveData = 4,
        CheckDescriptors = 5,
        CheckSN = 6
    }

    public class Command
    {
        public Operation Operation;
        public string Data;

        public Command(Operation operation, string data)
        {
            this.Operation = operation;
            this.Data = data;
        }
    }
}
