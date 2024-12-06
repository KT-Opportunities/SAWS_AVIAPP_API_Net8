namespace SAWSCore8API.Interfaces
{
    public interface IEmailService
    {
        public void Send(string to, string orgname, string fname, string lname);

        public void SendPasswordResetEmail(string to, string htmlBody);
        public void SendLogInCredentialsEmail(string to, string htmlBody);
    }
}
