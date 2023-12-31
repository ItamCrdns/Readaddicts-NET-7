﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostAPI.Interfaces;
using PostAPI.Models;

namespace PostAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly IComment _commentService;

        public CommentController(IComment commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{postId}")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CommentView>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetCommentsByPostId(int postId)
        {
            var comments = await _commentService.GetCommentsByPostId(postId);

            if (comments.Count == 0) return NoContent();

            return Ok(comments);
        }

        [HttpGet("id/{commentId}")]
        [ProducesResponseType(200, Type = typeof(Comment))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetCommendById(int commentId)
        {
            var comment = await _commentService.GetCommentViewById(commentId);

            if (comment == null) return NotFound();

            return Ok(comment);
        }

        [HttpPost("post/{postId}")]
        [Authorize(Policy = "UserAllowed")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateComment(int postId, int parentCommentId, Comment comment)
        {
            var validator = new CommentValidator();
            var validationResult = await validator.ValidateAsync(comment);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => new ValidationError
                {
                    Field = error.PropertyName,
                    Error = error.ErrorMessage
                }).ToList();

                return BadRequest(errors);
            }

            int newCommentId = await _commentService.CreateComment(postId, parentCommentId, comment);

            if (newCommentId == 0)
            {
                ModelState.AddModelError("", "Something went wrong");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest();

            return Ok(newCommentId);
        }

        [HttpPatch("update/{commentId}")]
        [Authorize(Policy = "UserAllowed")]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] Comment comment)
        {
            bool exists = await _commentService.CommentIdExists(commentId);

            if (!exists)
                return NotFound("The comment does not exist");

            var validator = new CommentValidator();
            var validatorResult = validator.Validate(comment);

            if (!validatorResult.IsValid)
            {
                var errors = validatorResult.Errors.Select(error => new ValidationError
                {
                    Field = error.PropertyName,
                    Error = error.ErrorMessage
                }).ToList();

                return BadRequest(errors);
            }

            var updated = await _commentService.UpdateComment(commentId, comment);

            if (!updated)
            {
                ModelState.AddModelError("", "Something went wrong");
                return StatusCode(500, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest();

            return Ok("Comment updated");
        }

        [HttpDelete("delete/{commentId}")]
        [Authorize(Policy = "UserAllowed")]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            bool exists = await _commentService.CommentIdExists(commentId);

            if (!exists)
                return NotFound("The comment does not exist or has already been deleted");

            var comment = await _commentService.GetCommentById(commentId);
            bool deleted = await _commentService.DeleteComment(comment);

            if (!deleted)
            {
                ModelState.AddModelError("", "Something went wrong");
                return StatusCode(500, ModelState);
            }

            return Ok("Comment deleted");
        }
    }
}
