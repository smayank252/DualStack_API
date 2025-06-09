from flask import Flask, render_template, request, redirect, flash, url_for, Response
from flask_login import LoginManager, UserMixin, login_user, login_required, logout_user
from werkzeug.security import generate_password_hash, check_password_hash
# from werkzeug.wrappers.response import Response as WerkzeugResponse
import requests
from typing import Dict, Any, Optional, Union, cast


app = Flask(__name__)
app.secret_key = 'your_secret_key'  # Required for Flask-Login


# Initialize Flask-Login
login_manager = LoginManager()
login_manager.init_app(app)
login_manager.login_view = 'login'  # Redirect to login page if unauthorized


# Replace this with your actual API URLs
UPLOAD_API_URL = "https://backend-gpa2hegrenaehzea.eastus2-01.azurewebsites.net/api/form"
SUBMISSION_API_URL = "https://backend-gpa2hegrenaehzea.eastus2-01.azurewebsites.net/api/form/submissions"


# Mock user database (replace with a real database in production)
users: Dict[str, Dict[str, Any]] = {
    "admin": {
        "id": 1,
        "username": "admin",
        "password": generate_password_hash("admin123")  # Hashed password
    }
}


# User class for Flask-Login
class User(UserMixin):
    def __init__(self, user_id: int, username: str) -> None:
        self.id = user_id
        self.username = username


# Load user callback for Flask-Login
@login_manager.user_loader
def load_user(user_id: str) -> Optional[User]:
    user_data = users.get("admin")  # Replace with a database query in production
    if user_data and user_data["id"] == int(user_id):
        return User(user_data["id"], user_data["username"])
    return None


# Login route
@app.route('/login', methods=["GET", "POST"])
def login() -> Union[Response, str]:
    if request.method == "POST":
        username = request.form.get("username", "")
        password = request.form.get("password", "")
        if not username or not password:
            flash("Username and password are required", "error")
            return render_template("login.html")

        user_data = users.get(username)
        if user_data and check_password_hash(user_data["password"], password):
            user = User(user_data["id"], user_data["username"])
            login_user(user)
            flash("Logged in successfully!", "success")

            # Redirect to the submissions page after login
            next_page = request.args.get('next')  # Get the 'next' parameter from the URL
            return cast(Response, redirect(next_page or url_for("submissions")))  # Redirect to submissions or the next page
        else:
            flash("Invalid username or password", "error")
    return render_template("login.html")


# Logout route
@app.route('/logout')
@login_required
def logout() -> Response:
    logout_user()
    flash("Logged out successfully!", "success")
    return cast(Response, redirect(url_for("login")))


# Home route (index.html - upload page)
@app.route('/')
def home() -> str:
    return render_template("index.html")


# Upload route
@app.route('/upload', methods=["POST"])
def upload() -> Response:
    photo = request.files.get("photo")
    if not photo:
        flash("Photo is required", "error")
        return cast(Response, redirect(url_for("home")))

    data = {
        "name": request.form.get("name", ""),
        "email": request.form.get("email", ""),
        "address": request.form.get("address", "")
    }

    # Send the photo and data to the backend API
    files = {"photo": (photo.filename, photo.stream, photo.mimetype)}
    response = requests.post(UPLOAD_API_URL, files=files, data=data)

    if response.status_code == 200:
        flash("Successfully uploaded!", "success")
    else:
        flash("Error uploading file", "error")

    return cast(Response, redirect(url_for("home")))


# Submissions route (protected)
@app.route('/submissions')
@login_required
def submissions() -> Union[str, tuple[str, int]]:
    # Fetch submissions from the backend API
    response = requests.get(SUBMISSION_API_URL)

    if response.status_code == 200:
        submissions = response.json()
        return render_template("submissions.html", submissions=submissions)
    else:
        return "Error fetching submissions", 500


if __name__ == "__main__":
    app.run(debug=True, port=5001)
