﻿using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TaskRouter.Models;
using Twilio.TaskRouter;

namespace TaskRouter.Controllers
{
    [RoutePrefix("home")]
    public class HomeController : ApiController
    {
        private static string _accountSid = ConfigurationManager.AppSettings["AccountSid"];
        private static string _authToken = ConfigurationManager.AppSettings["AuthToken"];
        private static string _workspaceSid = ConfigurationManager.AppSettings["WorkspaceSid"];
        private static string _workflowSid = ConfigurationManager.AppSettings["WorkflowSid"];

        private readonly TaskRouterClient _client = new TaskRouterClient(_accountSid, _authToken);

        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok("Twilio TaskRouter Demo Api says: 'Hello world!'");
        }

        [HttpPost]
        [Route("assignment_callback")]
        public IHttpActionResult AssignmentCallback(AssignmentCallback callback)
        {
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { instruction = "accept" }));
        }

        [HttpGet]
        [Route("createTask")]
        public IHttpActionResult CreateTask()
        {
            Task task = _client.AddTask(_workspaceSid, "{\"selected_language\":\"es\"}", _workflowSid, null, 0);
            if (task.RestException?.Message != null)
            {
                return BadRequest(task.RestException.Message);
            }

            return Ok($"Created task {task.Sid} with attributes {task.Attributes} at {DateTime.Now}");
        }

        [HttpPost]
        [Route("accept_reservation")]
        public IHttpActionResult AcceptReservation([FromUri] ReservationCallback callback)
        {
            var taskSid = callback.TaskSid;
            var reservationSid = callback.ReservationSid;

            Reservation reservation = _client.UpdateReservation(_workspaceSid, taskSid, reservationSid, "accepted", null);

            if (reservation.RestException?.Message != null)
            {
                return BadRequest(reservation.RestException.Message);
            }

            return Ok($"Accepted reservation for {taskSid} with status {reservation.ReservationStatus} at {DateTime.Now}");
        }
    }
}
