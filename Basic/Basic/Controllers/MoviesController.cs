using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using Basic.Models;

namespace Basic.Controllers
{
    public class MoviesController : Controller
    {

        private string connectionString;

        public MoviesController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["MusicStoreDB"].ConnectionString;
        }

        // GET: Movies
        public ActionResult Index()
        {
            List<Movie> movies = new List<Movie>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Movies";
                SqlCommand sqlCommand = new SqlCommand(query, conn);

                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        int ID = dataReader.GetInt32(dataReader.GetOrdinal("ID"));
                        string Title = dataReader.GetString(dataReader.GetOrdinal("Title"));
                        string Genre = dataReader.GetString(dataReader.GetOrdinal("Genre"));
                        string Year = dataReader.GetString(dataReader.GetOrdinal("Year"));
                        Movie movie = new Movie();
                        movie.ID = ID;
                        movie.Title = Title;
                        movie.Genre = Genre;
                        movie.Year = Year;
                        movies.Add(movie);
                    }
                }
 
            }

            ViewBag.movies = movies;

            return View();
        }
    }
}