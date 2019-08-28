import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  // {} means an empty object, it will store the User Credentials which are
  // eventually sent to the server
  model: any = {};

  constructor() { }

  ngOnInit() {
  }

  login() {
    console.log(this.model);
  }
}
