using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE.LetterAutomation
{
    public class Kelasehnameh
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("کد کلاسه")]
        public int CodeKelaseh { get; set; }

        [DisplayName("نام کلاسه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string? NameKelaseh { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
