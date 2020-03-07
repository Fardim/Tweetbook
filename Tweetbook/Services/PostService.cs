﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tweetbook.Data;
using Tweetbook.Domain;

namespace Tweetbook.Services
{
    public class PostService: IPostService
    {
        private readonly DataContext _context;
        public PostService(DataContext context)
        {
            _context = context;
        }
        public async Task<List<Post>> GetPostsAsync()
        {
            return await _context.Posts.ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(Guid postId)
        {
            return await _context.Posts.SingleOrDefaultAsync(d => d.Id == postId);
        }

        public async Task<bool> CreatePostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            var created = await _context.SaveChangesAsync();
            return created > 0;
        }
        public async Task<bool> UpdatePostAsync(Post postToUpdate)
        {
            _context.Posts.Update(postToUpdate);
            var updated = await _context.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var post = await GetPostByIdAsync(postId);
            if (post != null)
            {
                _context.Posts.Remove(post);
                var deleted = await _context.SaveChangesAsync();
                return deleted> 0;
            }
            return false;
        }

        public async Task<bool> UserOwnsPostAsync(Guid postId, string getUserId)
        {
            var post = await _context.Posts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                return false;
            }
            if (post.UserId != getUserId)
            {
                return false;
            }

            return true;
        }
    }
}
