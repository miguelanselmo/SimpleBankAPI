
-- DROP TABLE public.users CASCADE;
-- DROP TABLE public.accounts CASCADE;
-- DROP TABLE public.movements CASCADE;
-- DROP TABLE public.transfers CASCADE;
-- DROP TABLE public.sessions CASCADE;

CREATE TABLE public.users (
	id serial4 NOT NULL,
	username varchar NOT NULL,
	password varchar NOT NULL,
	full_name varchar NOT NULL,
	email varchar NOT NULL,
	--password_changed_at timestamptz NOT NULL DEFAULT now(),
	created_at timestamptz NOT NULL DEFAULT now(),
	CONSTRAINT users_email_key UNIQUE (email),
	CONSTRAINT users_pkey PRIMARY KEY (id),
	CONSTRAINT users_username UNIQUE (username)
);

CREATE TABLE public.accounts (
	id serial4 NOT NULL,
	user_id int NOT NULL,
	balance decimal NOT NULL,
	currency varchar NOT NULL,
	created_at timestamptz NOT NULL DEFAULT now(),
	CONSTRAINT accounts_pkey PRIMARY KEY (id),
	CONSTRAINT accounts_fkey FOREIGN KEY(user_id) REFERENCES users(id)
);

CREATE TABLE public.movements (
	id serial4 NOT NULL,
	account_id int NOT NULL,
	amount decimal NOT NULL,
	balance decimal NOT NULL,
	created_at timestamptz NOT NULL DEFAULT now(),
	CONSTRAINT movements_pkey PRIMARY KEY (id),
	CONSTRAINT movements_fkey FOREIGN KEY(account_id) REFERENCES accounts(id)
);
/*
CREATE TABLE public.transfers (
	id serial4 NOT NULL,
	from_account_id int NOT NULL,
	to_account_id int NOT NULL,
	amount decimal NOT NULL,
	created_at timestamptz NOT NULL DEFAULT now(),
	CONSTRAINT transfers_pkey PRIMARY KEY (id),
	CONSTRAINT transfers_fkey FOREIGN KEY(from_account_id) REFERENCES accounts(id),
	CONSTRAINT transfers_fkey2 FOREIGN KEY(to_account_id) REFERENCES accounts(id)
);
*/
CREATE TABLE public.operations_log (
	id serial4 NOT NULL,
	data json NOT NULL,
	created_at timestamptz NOT NULL DEFAULT now(),
	CONSTRAINT transfers_pkey PRIMARY KEY (id),
);

CREATE TABLE public.sessions (
	id varchar NOT NULL,
	user_id int NOT NULL,
	active bool NOT NULL DEFAULT true,
	created_at timestamptz NOT NULL DEFAULT now(),
	refresk_token varchar,
	refresk_token_expire_at timestamptz,
	CONSTRAINT sessions_fkey FOREIGN KEY(user_id) REFERENCES users(id)
);

CREATE TABLE public.documents (
	id varchar NOT NULL,
	account_id int NOT NULL,
	file_name varchar NOT NULL,
	uri varchar NOT NULL,
	content_type varchar NOT NULL,
	created_at timestamptz NOT NULL DEFAULT now(),
	CONSTRAINT documents_fkey FOREIGN KEY(account_id) REFERENCES accounts(id)
);


