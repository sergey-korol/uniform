using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using ServiceStack.DataAnnotations;
using Uniform.Mongodb;

namespace Uniform.Sample.Documents
{
    [Document] 
    [Alias("comments")]
    public class CommentDocument
    {
        [DocumentId, BsonId]
        public String CommentId { get; set; }

        [References(typeof(UserDocument))]
        public String UserId { get; set; }

        [References(typeof(QuestionDocument))]
        public String QuestionId { get; set; }
        public String Content { get; set; }
        
        [StringLength(6000)]
        public QuestionDocument QuestionDocument { get; set; }

        [StringLength(6000)]
        public List<VoteDocument> Votes { get; set; }

        public CommentDocument()
        {
            Votes = new List<VoteDocument>();
        }
    }
}