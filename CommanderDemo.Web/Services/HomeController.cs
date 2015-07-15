using System;
using System.Web.Mvc;
using CommanderDemo.Domain;
using MediatR;

namespace CommanderDemo.Web
{
    /// <summary>
    /// Note how the HomeController only has a single dependency on IMediator.
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Login()
        {
            return View(new LoginUser());
        }

        [HttpPost, AllowAnonymous]
        public ActionResult Login(LoginUser loginUser)
        {
            return Send(loginUser, x => RedirectToAction("Index"), View);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post), AllowAnonymous]
        public ActionResult Logout(LogoutUser logoutUser)
        {
            return Send(logoutUser, x => RedirectToAction("Index"));
        }

        [HttpGet]
        public ActionResult Users(QueryUsers queryUsers)
        {
            return Send(queryUsers, View);
        }

        [HttpGet]
        public ActionResult EditUser(GetUser getUser)
        {
            return Send(getUser, x => View("EditUser", x));
        }

        [HttpPost]
        public ActionResult EditUser(SaveUser saveUser)
        {
            return Send(saveUser, x => RedirectToAction("Users"), () => EditUser(new GetUser { Id = saveUser.Id }));
        }

        //Note: Only Admin can delete users due to the Authorize(Users="Admin") attribute, create another user and try it.
        [HttpGet]
        public ActionResult DeleteUser(DeleteUser deleteUser)
        {
            return Send(deleteUser, x => RedirectToAction("Users"), () => EditUser(new GetUser { Id = deleteUser.Id }));
        }

        /// <summary>
        /// Send is a wrapper for the Mediator call that provides a common way to put exceptions in the ViewBag
        /// which can be displayed by Shared\_Messages.
        /// </summary>
        private ActionResult Send<T>(IRequest<T> cmd, Func<T, ActionResult> success, Func<ActionResult> failure = null)
        {
            try
            {
                var response = _mediator.Send(cmd);
                return success(response);
            }
            catch (Exception ex)
            {
                ViewBag._Error = ex.Message;
                return failure == null ? success(default(T)) : failure();
            }
        }
    };
}
