FROM postgres:10.1

ENV POSTGRES_USER postgres
ENV POSTGRES_PASSWORD postgres
ENV POSTGRES_DB market_cap
ENV PGDATA /var/lib/postgresql/data/db-files/

COPY *.sql /docker-entrypoint-initdb.d/