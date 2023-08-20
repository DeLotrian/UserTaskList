namespace UserTask.DAL.Repository
{
    public class UserTaskDbConfigMongo
    {
        public string Database_Name { get; set; }
        public string Users_Collection_Name { get; set; }
        public string Task_Lists_Collection_Name { get; set; }
        public string Connection_String { get; set; }
    }
}