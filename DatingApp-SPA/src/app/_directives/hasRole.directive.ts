import { Directive, Input, ViewContainerRef, TemplateRef, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Directive({
  selector: '[appHasRole]' // Prefix a star when we use it because it's a structural directive: *appHasRole
})// we also need pass some parameters because we want to pass the Roles in order to access the elements we
  // are protecting with this. To pass the parameters, we need it as an @Input property:
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  isVisible = false; // show or hide the protected elements based on the Roles

  constructor(private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private authService: AuthService) { }

  ngOnInit(): void {
    const userRoles = this.authService.decodedToken.role as Array<string>;
    // if no roles clear the viewContainerRef
    if (!userRoles) {
      this.viewContainerRef.clear();
    }

    // if user has role needed then render the element
    if (this.authService.roleMatch(this.appHasRole)) {
      if (!this.isVisible) {
        this.isVisible = true;
        // this.templateRef refers to the element our structural directive is applying to
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      } else {
        this.isVisible = false;
        this.viewContainerRef.clear();
      }
    }
  }
}
