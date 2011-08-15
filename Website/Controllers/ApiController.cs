﻿using System;
using System.Web.Mvc;
using NuGet;

namespace NuGetGallery {
    public partial class ApiController : Controller {
        readonly IPackageService packageSvc;
        readonly IUserService userSvc;

        public ApiController(IPackageService packageSvc, IUserService userSvc) {
            this.packageSvc = packageSvc;
            this.userSvc = userSvc;
        }

        [ActionName("PushPackageApi"), HttpPost]
        public virtual ActionResult CreatePackage(Guid apiKey) {
            var user = userSvc.FindByApiKey(apiKey);
            if (user == null)
                throw new EntityException(Strings.ApiKeyNotAuthorized, "push");

            var packageToPush = ReadPackageFromRequest();

            var package = packageSvc.FindPackageByIdAndVersion(packageToPush.Id, packageToPush.Version.ToString());
            if (package != null)
                throw new EntityException(Strings.PackageExistsAndCannotBeModified, packageToPush.Id, packageToPush.Version.ToString());

            package = packageSvc.CreatePackage(packageToPush, user);
            return new EmptyResult();
        }

        [ActionName("DeletePackageApi"), HttpDelete]
        public virtual ActionResult DeletePackage(Guid apiKey, string id, string version) {
            var user = userSvc.FindByApiKey(apiKey);
            if (user == null)
                throw new Exception("The specified API key does not provide the authority to push packages.");

            var package = packageSvc.FindPackageByIdAndVersion(id, version);
            if (package == null)
                throw new Exception(string.Format("A package with id '{0}' and version '{1}' does not exist.", id, version));

            packageSvc.DeletePackage(id, version);
            return new EmptyResult();
        }

        [ActionName("PublishPackageApi"), HttpPost]
        public virtual ActionResult PublishPackage(Guid key, string id, string version) {
            var user = userSvc.FindByApiKey(key);
            if (user == null)
                throw new Exception("The specified API key does not provide the authority to push packages.");

            var package = packageSvc.FindPackageByIdAndVersion(id, version);
            if (package == null)
                throw new Exception(string.Format("A package with id '{0}' and version '{1}' does not exist.", id, version));

            packageSvc.PublishPackage(id, version);
            return new EmptyResult();
        }

        public virtual IPackage ReadPackageFromRequest() {
            return new ZipPackage(Request.InputStream);
        }
    }
}