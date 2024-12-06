namespace SAWSCore8API.Models
{
    public class CreateResult : UpdateResult
    {
        public int NewId { get; set; }

        public static CreateResult SuccessResult(int id)
        {
            return new CreateResult
            {
                Success = true,
                NewId = id,
            };
        }
    }
}
