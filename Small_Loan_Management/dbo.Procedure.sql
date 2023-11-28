CREATE PROCEDURE [dbo].InsertLoan
	@CustomerName NVARCHAR(50), @Address NVARCHAR(150), @Phone NVARCHAR(20), @Guarantor NVARCHAR(50),@GuarantorAddress NVARCHAR(150), @LoanAmount MONEY, 
                                           @ApprovalDate DATE, @Tenure INT,@TotalPayable MONEY, @CustomerPicture NVARCHAR(30),
										   @id INT OUTPUT
AS

	INSERT INTO Loans(CustomerName, [Address], Phone,Guarantor, GuarantorAddress, LoanAmount, ApprovalDate, Tenure, TotalPayable, CustomerPicture)
	VALUES (@CustomerName, @Address,  @Phone,@Guarantor,@GuarantorAddress, @LoanAmount, @ApprovalDate, @Tenure, @TotalPayable, @CustomerPicture)
RETURN 0
