# Use an official Python runtime as a parent image
FROM python:3.9-slim

# Set the working directory in the container
WORKDIR /app

# Copy the requirements file into the container
COPY requirements.txt .

# Install any needed packages specified in requirements.txt
RUN pip install --no-cache-dir -r requirements.txt

# Copy the rest of the application code into the container
COPY . .

# Make port 5001 available to the world outside this container
EXPOSE 5001

# Define environment variable for development
ENV FLASK_APP=app.py
ENV FLASK_ENV=development

# Run the Flask application on port 5001
CMD ["flask", "run", "--host=0.0.0.0", "--port=5001"]