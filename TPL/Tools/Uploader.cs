//namespace TPLWeb.Tools

//{
//    public class Uploader
//    {
//        private readonly IWebHostEnvironment _environment;

//        public Uploader(IWebHostEnvironment webHostEnvironment)
//        {
//            _environment = webHostEnvironment;
//        }


//        //public string AtbaUploader(IFormFile? file, string guid)
//        //{
//        //    if (file == null || string.IsNullOrEmpty(guid))
//        //    {
//        //        return null!;
//        //    }

//        //    var dir = Path.Combine(_environment.WebRootPath, "images", "Atba", guid);
//        //    if (!Directory.Exists(dir))
//        //    {
//        //        Directory.CreateDirectory(dir);
//        //    }

//        //    var extension = Path.GetExtension(file.FileName);
//        //    var path = Path.Combine(dir, $"{guid}{extension}");
//        //    using var f = new FileStream(path, FileMode.Create);
//        //    file.CopyTo(f);

//        //    return $"{guid}{extension}";
//        //}
//        //public string AtbaFacesidesUploader(IFormFile? file, string guid, string side)
//        //{
//        //    if (file == null || string.IsNullOrEmpty(guid))
//        //    {
//        //        return null!;
//        //    }

//        //    var dir = Path.Combine(_environment.WebRootPath, "images", "Atba", guid);
//        //    if (!Directory.Exists(dir))
//        //    {
//        //        Directory.CreateDirectory(dir);
//        //    }

//        //    var extension = Path.GetExtension(file.FileName);
//        //    var path = Path.Combine(dir, $"{guid}{side}{extension}");
//        //    using var f = new FileStream(path, FileMode.Create);
//        //    file.CopyTo(f);

//        //    return $"{guid}{side}{extension}";
//        //}
//        //public string AtbaFingersUploader(IFormFile? file, string guid, int count)
//        //{
//        //    if (file == null || string.IsNullOrEmpty(guid))
//        //    {
//        //        return null!;
//        //    }

//        //    var dir = Path.Combine(_environment.WebRootPath, "images", "Atba", guid);
//        //    if (!Directory.Exists(dir))
//        //    {
//        //        Directory.CreateDirectory(dir);
//        //    }

//        //    var extension = Path.GetExtension(file.FileName);
//        //    var path = Path.Combine(dir, $"{guid}_Fingers_{count}{extension}");
//        //    using var f = new FileStream(path, FileMode.Create);
//        //    file.CopyTo(f);

//        //    return $"{guid}_Fingers_{count}{extension}";
//        //}



//    }
//}