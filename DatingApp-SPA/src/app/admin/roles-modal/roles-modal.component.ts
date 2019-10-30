import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
// child component of UserManagementComponent
export class RolesModalComponent implements OnInit {
  @Output() updateSelectedRoles = new EventEmitter();
  user: User;   // will be auto-filled by parent due to the ngx-bootstrap Modal
  roles: any[]; // ditto

  constructor(public bsModalRef: BsModalRef) {}

  ngOnInit() { }

  updateRoles() {
    this.updateSelectedRoles.emit(this.roles);
    this.bsModalRef.hide();
  }
}
