using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal;
using Microsoft.AspNetCore.Mvc;
using Tweetbook.Contracts;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Domain;
using Tweetbook.Extensions;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TagsController : Controller
    {
        private readonly IPostService _postService;

        public TagsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet(ApiRoutes.Tags.GetAll)]
        //[Authorize(Policy = "TagViewer")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _postService.GetTagsAsync());
        }

        [HttpGet(ApiRoutes.Tags.Get)]
        public async Task<IActionResult> Get([FromRoute]string tagName)
        {
            var tag = await _postService.GetTagByNameAsync(tagName);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        [HttpDelete(ApiRoutes.Tags.Delete)]
        //[Authorize(Roles = "Admin")]
        [Authorize(Policy = "MustWorkForKaiser")]
        public async Task<IActionResult> Delete([FromRoute] string tagName)
        {
            var deleted = await _postService.DeleteTagAsync(tagName);
            if (deleted)
                return NoContent();
            return NotFound();
        }

        [HttpPost(ApiRoutes.Tags.Create)]
        public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
        {
            var newTag = new Tag
            {
                Name = request.TagName,
                CreateOn = DateTime.Now,
                CreatorId = HttpContext.GetUserId()
            };
            var created = await _postService.CreateTagAsync(newTag);
            if (!created)
            {
                return BadRequest(new {Message = "Unable to create tag"});
            }

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Tags.Get.Replace("{tagName}", newTag.Name);
            return Created(locationUri, newTag);
        }
    }
}