﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mytest.Data;
using System;
using System.Linq;

namespace mytest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        //private readonly string sortOrder;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index(string sort, string search)
        {
            ViewBag.AuthorSort = String.IsNullOrEmpty(sort) ? "Author" : "";
            
            switch (sort)
            {
                case "Author":
                    return View(_context.Books.OrderBy(x => x.Author));
            }
            

            if (search == null)
                return View(_context.Books.ToList());
            else
            {
                return View(_context.Books.Where(x => x.Title.ToLower().Contains(search.ToLower()) ||
                                                             x.Author.ToLower().Contains(search.ToLower()) ||
                                                             x.Genre.ToLower().Contains(search.ToLower()) ||
                                                             x.ISBN.Contains(search) ||
                                                             x.Publisher.ToLower().Contains(search.ToLower()) ||
                                                             search == null).ToList());
            }
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Team 5";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [Authorize]
        public IActionResult ShowBook(string field)
        {
            if (field == null)
                return View();
            else
            {
                ViewBag.BookISBN = field;
                var comments = _context.Comments.Where(d => d.BookISBN.Equals(field)).ToList();
                ViewBag.Comments = comments;

                var ratings = _context.Comments.Where(d => d.BookISBN.Equals(field)).ToList();

                if (ratings.Count() > 0)
                {
                    var ratingSum = ratings.Sum(d => d.Rating.Value);
                    ViewBag.RatingSum = ratingSum;
                    var ratingCount = ratings.Count();
                    ViewBag.RatingCount = ratingCount;
                }
                else
                {
                    ViewBag.RatingSum = 0;
                    ViewBag.RatingCount = 0;
                }

                return View(_context.Books.Where(x => x.ISBN.Equals(field) || field == null).ToList());
            }
        }
        [Authorize]
        public IActionResult ShowAuthor(string field)
        {
            if (field == null)
                return View();
            else
            {
                return View(_context.Books.Where(x => x.Author.ToLower().Equals(field.ToLower()) || field == null).ToList());
            }
        }
    }
}
