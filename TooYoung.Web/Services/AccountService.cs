﻿using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TooYoung.Models;

namespace TooYoung.Services
{
    public class AccountService
    {
        public IMongoCollection<User> Users { get; set; }

        public AccountService(IMongoDatabase db)
        {
            Users = db.GetCollection<User>(nameof(User));
        }

        /// <summary>
        /// 根据用户登录名查找用户信息
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<User> FindByUserName(string username)
        {
            return await Users.AsQueryable().FirstOrDefaultAsync(u => u.UserName == username);
        }

        /// <summary>
        /// 插入一个新的用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<User> Create(User user)
        {
            await Users.InsertOneAsync(user);
            return user;
        }
    }
}