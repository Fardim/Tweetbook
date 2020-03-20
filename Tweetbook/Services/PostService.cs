using System;
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
            return await _context.Posts.Include(d=> d.Tags).ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(Guid postId)
        {
            return await _context.Posts.Include(d=> d.Tags).SingleOrDefaultAsync(d => d.Id == postId);
        }

        public async Task<bool> CreatePostAsync(Post post)
        {
            post.Tags?.ForEach(x=> x.TagName = x.TagName.ToLower());
            await AddNewTags(post);
            await _context.Posts.AddAsync(post);
            var created = await _context.SaveChangesAsync();
            return created > 0;
        }

        private async Task AddNewTags(Post post)
        {
            foreach (var tag in post.Tags)
            {
                var existingTag = await _context.Tags.SingleOrDefaultAsync((x => x.Name == tag.TagName));
                if(existingTag != null)
                    continue;
                await _context.Tags.AddAsync(new Tag
                    {Name = tag.TagName, CreateOn = DateTime.Now, CreatorId = post.UserId});
            }
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

        public async Task<List<Tag>> GetTagsAsync()
        {
            return await _context.Tags.ToListAsync();
        }

        public async Task<Tag> GetTagByNameAsync(string tagName)
        {
            return await _context.Tags.SingleOrDefaultAsync(d => d.Name == tagName);
        }

        public async Task<bool> CreateTagAsync(Tag tag)
        {
            tag.Name = tag.Name.ToLower();
            var existingTag = await _context.Tags.AsNoTracking().SingleOrDefaultAsync(d => d.Name == tag.Name);
            if(existingTag!=null)
            {
                return true;
            }

            await _context.Tags.AddAsync(tag);
            var created = await _context.SaveChangesAsync();
            return created > 0;
        }

        public async Task<bool> DeleteTagAsync(string tagName)
        {
            var tag = await GetTagByNameAsync(tagName);
            if (tag != null)
            {
                _context.Tags.Remove(tag);
                var deleted = await _context.SaveChangesAsync();
                return deleted > 0;
            }
            return false;
        }
    }
}
