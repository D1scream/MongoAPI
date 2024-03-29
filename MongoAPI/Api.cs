using MongoAPI.Controllers;
using MongoAPI.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace MongoAPI
{
    public class Api
    {
        public Api()
        {
        }
        public void Start(string[] args)
        {
            var builder = WebApplication.CreateBuilder();
            var app = builder.Build();
            app.Run(async (context) =>
            {
                var response = context.Response;
                var request = context.Request;
                var path = request.Path;
                string pathStr = path;
                var expressionForNumber = "/movies/([0-9]+)$";   // если id представляет число
                var expressionForString = "/movies/([a-b]+)$";   // если id представляет строку
                
                MongoApiController mongoApiController = new MongoApiController("ShadrinDB","Movies");
                if (path == "/movies" && request.Method == "GET")
                {
                    await mongoApiController.GetMovies(response, request);
                }
                
                else if (path == "/movies" && request.Method == "POST")
                {
                    await mongoApiController.AddMovie(response, request);
                }
                else if (path == "/movies" && request.Method == "PUT")
                {
                    await mongoApiController.UpdateMovie(response, request);
                }
                else if (path.ToString().Split("/")[1] == "movies" && request.Method == "DELETE")
                {
                    string? id = path.ToString().Split("/")[2];
                    await mongoApiController.DeleteMovie(id, response);
                }
                else
                {
                    response.StatusCode = 404;
                    await response.WriteAsJsonAsync(new { message = "page not found" });

                }
            });
            /*
            app.MapGet("/api/movies/{id}", async (string id) =>
            {
                Console.WriteLine(id);
                var user = await db.GetCollection<Movie>(collectionName)
                    .Find(p => p._id == id)
                    .FirstOrDefaultAsync();
                if (user == null) return Results.NotFound(new { message = "Кино не найдено" });

                return Results.Json(user);
            });
            app.MapDelete("/api/movies/{id}", async (string id) =>
            {
                var user = await db.GetCollection<Movie>(collectionName).FindOneAndDeleteAsync(p => p._id == id);
                if (user is null) return Results.NotFound(new { message = "Кино не найдено" });
                return Results.Json(user);
            });

            app.MapPost("/api/movies", async (Movie user) => {

                await db.GetCollection<Movie>(collectionName).InsertOneAsync(user);
                return user;
            });

            app.MapPut("/api/movies", async (Movie userData) => {

                Movie user = await db.GetCollection<Movie>(collectionName)
                    .FindOneAndReplaceAsync(p => p._id == userData._id, userData, new() { ReturnDocument = ReturnDocument.After });
                if (user == null)
                    return Results.NotFound(new { message = "Кино не найдено" });
                return Results.Json(user);
            });
            */
            app.Run();
            
        }

    }
    
}
