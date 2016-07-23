using redis_example2.Models;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace redis_example2.Controllers
{
    public class UsersController : Controller
    {
        // GET: Users
        public ActionResult NewUser()
        {
            return View();
        }
        public ActionResult Save(string userName, int goal, long? userId)
        {
            User user;
            using (IRedisClient client = new RedisClient())
            {
                var userClient = client.As<User>();
                if (userId != null)
                {
                    user = userClient.GetById(userId);
                    client.RemoveItemFromSortedSet("urn:leaderboard", user.Name);
                }
                else
                {
                    user = new User
                    {
                        Id = userClient.GetNextSequence()
                    };
                }
                user.Goal = goal;
                user.Name = userName;
                userClient.Store(user);
                client.AddItemToSortedSet("urn:leaderboard", user.Name, user.Total);
            }

            return RedirectToAction("Index", "Tracker", new { userId = user.Id });
        }

        public ActionResult Edit(int userId)
        {
            using(IRedisClient client = new RedisClient())
            {
                var userClient = client.As<User>();
                var user = userClient.GetById(userId);
                ViewBag.UserName = user.Name;
                ViewBag.Goal = user.Goal;
                ViewBag.UserId = user.Id;
            }
            return View("NewUser");
        }
    }
}