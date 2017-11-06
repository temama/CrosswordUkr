-- Constraints
alter table users add constraint UC_users unique (login)

alter table words add constraint UC_words unique (TheWord)

-- Users
insert into users (login, initials, Email, emailconfirmed, passwordhash, roles, valid, securitystamp,settings)
values ('System', 'SYS', null, 0, 'SYSUSERPASS', 'Admin', 1, null, '')

insert into users (login, initials, Email, emailconfirmed, passwordhash, roles, valid, securitystamp,settings)
values ('Temama', 'AR', null, 0, '', 'Admin;UsersAdmin', 1, null, '')

-- Versions
-- BEFORE RUN UPDATE USERID & VERSIONID
insert into versions (Name, Description, InternalNotes, State, LastModified, UserId)
values ('0.1.0', 'Initial version', 'Deep beta', 0, GetDate(), 1)

insert into settings (Name, Value, LastModified, UserId)
values ('CurrentTestingVersion', '1', GetDate(), 1)
