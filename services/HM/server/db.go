package main

import (
	"database/sql"
	"fmt"
	_ "github.com/mattn/go-sqlite3"
	"log"
	"os"
)
const (
	DbName = "./health.db" 
	CreateTable = "CREATE TABLE healthIndices(id integer not null primary key AUTOINCREMENT, weight integer, bp integer, pulse integer, walking_distance integer, comment text); DELETE FROM healthIndices;"
	InsertValues = "INSERT INTO healthIndices(weight, bp, pulse, walking_distance, comment) VALUES (?, ?, ?, ?, ?)"
	InsertValue = "INSERT INTO healthIndices(weight, bp, pulse, walking_distance, comment) VALUES (?, ?, ?, ?, ?)"
	SelectRows = "SELECT id, weight, bp, pulse, walking_distance, comment FROM healthIndices"
)

func tryAddMetrics(m *HealthMetrics) (bool, int64) {

	db, err := sql.Open("sqlite3", DbName)
	if err != nil {
		log.Fatal("Error while connecting to db: ", err)
	}
	defer db.Close()
	
	stmt, err := db.Prepare(InsertValues)
	if err != nil {
		log.Fatal(err)
		return false, -1
	}
	
	res, err := stmt.Exec(m.Weight, m.BloodPressure, m.Pulse, m.WalkingDistance, m.Comment) //todo
	if err != nil {
		log.Fatal(err)
		return false, -1
	}
	id, err := res.LastInsertId()
	if err != nil {
		log.Fatal(err)
		return false, -1
	}
	
	return true, id
}

func tryGetUserMetrics(uId string) (bool, []HealthMetrics) {

	//todo: uId
	var res []HealthMetrics
	
	db, err := sql.Open("sqlite3", DbName)
	if err != nil {
		log.Fatal("Error while connecting to db: ", err)
		return false, nil
	}
	defer db.Close()
	
	 rows, err := db.Query(SelectRows)
	 if err != nil {
		log.Fatal(err)
		return false, nil
	 }
	 defer rows.Close()
	 
	 for rows.Next() {
		var id int
		var weight int
		var bp int
		var pulse int
		var wd int
		var comment string
		rows.Scan(&id, &weight, &bp, &pulse, &wd, &comment)
		m := &HealthMetrics{weight, bp, pulse, wd, comment}
		res = append(res, *m)
	 }
	 return true, res
}

func prepareDb() {
	os.Remove(DbName)

	db, err := sql.Open("sqlite3", DbName)
	if err != nil {
		log.Fatal("Error while connecting to db: ", err)
	}
	defer db.Close()

	_, err = db.Exec(CreateTable)
	if err != nil {
		log.Printf("%q: %s\n", err, CreateTable)
		return
	}

	 tx, err := db.Begin()
	 if err != nil {
		log.Fatal(err)
	 }
	 stmt, err := tx.Prepare(InsertValues)
	 if err != nil {
		log.Fatal(err)
	 }
	 defer stmt.Close()
	 
	 for i := 0; i < 100; i++ {
	 _, err = stmt.Exec(i, i*3, i+3, (i-1)*4, fmt.Sprintf("Comment number %03d", i))
	 if err != nil {
		 log.Fatal(err)
	 }
	 }
	 tx.Commit()

	 rows, err := db.Query(SelectRows)
	 if err != nil {
		log.Fatal(err)
	 }
	 defer rows.Close()
	 
	 for rows.Next() {
		var id int
		var comment string
		rows.Scan(&id, &comment)
		fmt.Println(id, comment)
	 }

	// stmt, err = db.Prepare("select name from foo where id = ?")
	// if err != nil {
		// log.Fatal(err)
	// }
	// defer stmt.Close()
	// var name string
	// err = stmt.QueryRow("3").Scan(&name)
	// if err != nil {
		// log.Fatal(err)
	// }
	// fmt.Println(name)

	// _, err = db.Exec("delete from foo")
	// if err != nil {
		// log.Fatal(err)
	// }

	// _, err = db.Exec("insert into foo(id, name) values(1, 'foo'), (2, 'bar'), (3, 'baz')")
	// if err != nil {
		// log.Fatal(err)
	// }

	// rows, err = db.Query("select id, name from foo")
	// if err != nil {
		// log.Fatal(err)
	// }
	// defer rows.Close()
	// for rows.Next() {
		// var id int
		// var name string
		// rows.Scan(&id, &name)
		// fmt.Println(id, name)
	// }
}

func checkErr(err error) {
    if err != nil {
        panic(err)
    }
}
