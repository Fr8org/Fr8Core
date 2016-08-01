// TODO: FR-4943, remove this.
// using System.ComponentModel.DataAnnotations;
// 
// namespace Data.Entities
// {
// 	public class WebServiceDO : BaseObject
// 	{
// 		[Key]
// 		public int Id { get; set; }
// 		public string Name { get; set; }
// 		public string IconPath { get; set; }
// 
// 	    protected bool Equals(WebServiceDO other)
// 	    {
// 	        return Id == other.Id;
// 	    }
// 
// 	    public override bool Equals(object obj)
// 	    {
// 	        if (ReferenceEquals(null, obj)) return false;
// 	        if (ReferenceEquals(this, obj)) return true;
// 	        if (obj.GetType() != this.GetType()) return false;
// 	        return Equals((WebServiceDO) obj);
// 	    }
// 
// 	    public override int GetHashCode()
// 	    {
// 	        return Id;
// 	    }
// 	}
// }