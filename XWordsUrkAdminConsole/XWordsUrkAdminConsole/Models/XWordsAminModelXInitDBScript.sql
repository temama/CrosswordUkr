-- Constraints
alter table users add constraint UC_users unique (login)

alter table words add constraint UC_words unique (TheWord)

-- Users
insert into users (login, initials, Email, emailconfirmed, passwordhash, roles, valid, securitystamp,settings)
values ('System', 'SYS', null, 0, '', 'Admin', 1, null, '')

insert into users (login, initials, Email, emailconfirmed, passwordhash, roles, valid, securitystamp,settings)
values ('Temama', 'AR', null, 0, '', 'Admin;UsersAdmin', 1, null, '')

